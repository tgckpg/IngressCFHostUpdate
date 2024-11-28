package controllers

import (
	"io"
	"net/http"

	admissionv1 "k8s.io/api/admission/v1"
	networkingv1 "k8s.io/api/networking/v1"
	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"

	providers "github.com/tgckpg/ingress-dns-sync/providers"

	"github.com/emicklei/go-restful/v3"
	"k8s.io/apimachinery/pkg/runtime"
	"k8s.io/apimachinery/pkg/runtime/serializer"
	"k8s.io/apiserver/pkg/server/httplog"
	"k8s.io/klog/v2"
)

var (
	scheme = runtime.NewScheme()
	codecs = serializer.NewCodecFactory(scheme)
	deserializer = codecs.UniversalDeserializer()
)

var statusesNoTracePred = httplog.StatusIsNot(
    http.StatusOK,
    http.StatusFound,
    http.StatusMovedPermanently,
    http.StatusTemporaryRedirect,
    http.StatusBadRequest,
    http.StatusNotFound,
    http.StatusSwitchingProtocols,
)

func init() {
	admissionv1.AddToScheme(scheme)
	networkingv1.AddToScheme(scheme)
}

type Server struct {
	restfulCont *restful.Container
	Provider providers.Provider
}

func (s *Server) ServeHTTP(w http.ResponseWriter, req *http.Request) {
	handler := httplog.WithLogging(s.restfulCont, statusesNoTracePred)
	handler.ServeHTTP(w, req)
}

func ( s *Server ) Initialize() {
	s.restfulCont = restful.NewContainer() 

	ws := new(restful.WebService)

	ws.Path("/validating-webhook").
		Consumes(restful.MIME_JSON).
		Produces(restful.MIME_JSON)
	ws.Route(ws.POST("").To(s.triggerAdmission).
		Reads(admissionv1.AdmissionReview{}).
		Writes(admissionv1.AdmissionResponse{}))

	s.restfulCont.Add(ws)

	ws = new(restful.WebService)
	ws.Path("/sync").
		Produces(restful.MIME_JSON)
	ws.Route(ws.GET("").To(s.syncHosts).
		Writes(metav1.Status{}))

	s.restfulCont.Add(ws)
}

func ( s *Server ) syncHosts(request *restful.Request, response *restful.Response) {
	if s.Provider != nil {
		go func() {
			s.Provider.SyncHosts()
		}()
	}
	response.WriteEntity(metav1.Status{ Message: "OK" })
}

func ( s *Server ) triggerAdmission(request *restful.Request, response *restful.Response) {
	body, err := io.ReadAll(request.Request.Body)
	if err != nil {
		response.WriteError(http.StatusBadRequest, err)
		return
	}

	var admissionReviewReq admissionv1.AdmissionReview
	_, _, err = deserializer.Decode(body, nil, &admissionReviewReq)
	if err != nil {
		response.WriteError(http.StatusBadRequest, err)
		return
	}

	resp := admissionv1.AdmissionReview{
		TypeMeta: metav1.TypeMeta{
			Kind:       "AdmissionReview",
			APIVersion: "admission.k8s.io/v1",
		},
		Response: &admissionv1.AdmissionResponse{
			UID:     admissionReviewReq.Request.UID,
			Allowed: true,
			Result: &metav1.Status{ Message: "OK" },
		},
	}

	// Write out asap as we do not want to hinge the admission process
	response.WriteEntity(resp)

	var ingress networkingv1.Ingress
	if _, _, err := deserializer.Decode(admissionReviewReq.Request.Object.Raw, nil, &ingress); err != nil {
		klog.V(1).Info("skipping non-ingress resources")
		return
	}

	klog.InfoS("Received Admission",
		"uid", admissionReviewReq.Request.UID,
		"kind", admissionReviewReq.Request.Kind.Kind,
		"name", ingress.Name,
	)

	if s.Provider != nil {
		go func () {
			for _, rule := range ingress.Spec.Rules {
				s.Provider.UpdateHost( rule.Host, *admissionReviewReq.Request.DryRun )
			}
		}()
	}
}
