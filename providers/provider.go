package providers

import (
	"strings"
)

type Provider interface {
	Initialize(c *KubeClient) error
	SyncHosts()
	UpdateHost(host string, dryRun bool)
}

func getZoneFromHost(host string) string {
    _segs := strings.Split(strings.TrimSuffix(host, "."), ".")
    _segs = _segs[len(_segs)-2:]
    return strings.Join(_segs, ".")
}
