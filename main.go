package main

import (
	"flag"
	"net"
	"net/http"
	"os"
	"time"
	"github.com/spf13/pflag"
	"k8s.io/klog/v2"

	controllers "github.com/tgckpg/ingress-dns-sync/controllers"
	providers "github.com/tgckpg/ingress-dns-sync/providers"
)

type Config struct {
	// tlsCertFile is the file containing x509 Certificate for HTTPS. (CA cert,
	// Default: ""
	TLSCertFile string `json:"tlsCertFile,omitempty"`
	// tlsPrivateKeyFile is the file containing x509 private key matching tlsCertFile.
	// Default: ""
	TLSPrivateKeyFile string `json:"tlsPrivateKeyFile,omitempty"`
}

func main() {
	defer klog.Flush()

	fs := pflag.NewFlagSet("", pflag.ExitOnError)
	c := Config{}

	fs.StringVar(&c.TLSCertFile, "tls-cert-file", c.TLSCertFile,
	"File containing x509 Certificate used for serving HTTPS (with intermediate certs, if any, concatenated after server cert).")
	fs.StringVar(&c.TLSPrivateKeyFile, "tls-private-key-file", c.TLSPrivateKeyFile, "File containing x509 private key matching --tls-cert-file.")

	klog.InitFlags(nil)

	flag.Set("logtostderr", "true")
	pflag.CommandLine.AddFlagSet(fs)
	pflag.CommandLine.AddGoFlagSet(flag.CommandLine)
	pflag.Parse()

	address, port := "", "443"
	klog.InfoS("Starting to listen", "address", address, "port", port)
	server := controllers.Server{}

	server.Initialize()
	selectProvider(&server)

	s := &http.Server{
		Addr:           net.JoinHostPort(address, port),
		Handler:        &server,
		IdleTimeout:    90 * time.Second,
		ReadTimeout:    4 * 60 * time.Minute,
		WriteTimeout:   4 * 60 * time.Minute,
		MaxHeaderBytes: 1 << 20,
	}

	if c.TLSCertFile != "" {
		if err := s.ListenAndServeTLS(c.TLSCertFile, c.TLSPrivateKeyFile); err != nil {
			klog.ErrorS(err, "Failed to listen and serve")
			os.Exit(1)
		}
	} else if err := s.ListenAndServe(); err != nil {
		klog.ErrorS(err, "Failed to listen and serve")
		os.Exit(1)
	}
}

func selectProvider(s *controllers.Server) {

	kc := providers.KubeClient{}
	err := kc.Initialize()
	var provider providers.Provider
	if err != nil {
		goto passiveMode
	}

	err = kc.SelectLoadBalancer()
	if err != nil {
		goto passiveMode
	}

	provider = &providers.CloudflareSync{}
	err = provider.Initialize(&kc)
	if err == nil {
		s.Provider = provider
		return
	}

	passiveMode:
	klog.ErrorS( err, "Unable to determine the ExternalIP. Entering passive mode" )
}
