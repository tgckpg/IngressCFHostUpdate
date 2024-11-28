package providers

import (
	"context"
	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
	"k8s.io/klog/v2"

	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
)

type KubeClient struct {
	client *kubernetes.Clientset
	ExternalIP string
}

// Check for all namespace
var KubeNamespace string = ""

func ( c *KubeClient ) Initialize() error {

	config, err := rest.InClusterConfig()
	if err != nil {
		return err
	}

	cl, err := kubernetes.NewForConfig(config)
	if err != nil {
		return err
	}
	c.client = cl

	return nil
}

func ( c *KubeClient ) GetIngressHosts() *map[string]string {
	ctx := context.Background()
	hosts := map[string]string{}

	ingresses, err := c.client.NetworkingV1().Ingresses(KubeNamespace).List(ctx, metav1.ListOptions{})
	if err != nil {
		klog.ErrorS( err, "Cannot get ingress resources" )
		return &hosts
	}

	for _, ingress := range ingresses.Items {
		for _, rule := range ingress.Spec.Rules {
			hosts[rule.Host] = "1"
		}
	}

	return &hosts
}

func ( c *KubeClient ) SelectLoadBalancer() error {
	ctx := context.Background()

	services, err := c.client.CoreV1().Services(KubeNamespace).List(ctx, metav1.ListOptions{})
	if err != nil {
		return err
	}

	for _, svc := range services.Items {
		for _, ingressLB := range svc.Status.LoadBalancer.Ingress {
			klog.InfoS( "Selected LoadBalancer", "name", ingressLB.Hostname, "addr", ingressLB.IP )
			c.ExternalIP = ingressLB.IP
			return nil
		}
	}

	return nil
}
