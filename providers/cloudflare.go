package providers

import (
	"context"
	"os"

	"github.com/cloudflare/cloudflare-go"

	"k8s.io/klog/v2"
)

type CloudflareSync struct {
	api *cloudflare.API
	kube *KubeClient
}

func ( cf *CloudflareSync ) Initialize(c *KubeClient) error {
	cf.kube = c
	api, err := cloudflare.NewWithAPIToken(os.Getenv("CLOUDFLARE_API_TOKEN"))
	if err == nil {
		cf.api = api
	}
	return err
}

func ( cf *CloudflareSync ) SyncHosts() {
	ctx := context.Background()

	zones, err := cf.api.ListZones(ctx)
	if err != nil {
		klog.ErrorS( err, "Unable to get zones data" )
		return
	}

	ingressHosts := cf.kube.GetIngressHosts()

	for _, zone := range zones {

		ARecords := cloudflare.ListDNSRecordsParams{ Type: "A", Content: cf.kube.ExternalIP }
		rc := cloudflare.ZoneIdentifier(zone.ID)

		dnsRecords, _, err := cf.api.ListDNSRecords( ctx, rc, ARecords )
		if err != nil {
			klog.ErrorS( err, "Cannot get DNSRecords", "zone", zone.Name )
			continue
		}

		cfHosts := map[string]*cloudflare.DNSRecord{}
		for _, record := range dnsRecords {
			cfHosts[record.Name] = &record
		}

		for host, _ := range (*ingressHosts) {
			if record, ok := cfHosts[host]; ok {
				if record.Content != cf.kube.ExternalIP {
					klog.InfoS(
						"Updating A Record",
						"name", record.Name,
						"from", record.Content,
						"to", cf.kube.ExternalIP,
					)
					cf.updateRecord( &ctx, record, rc )
				}
			} else {
				cf.createOrUpdateHost( &ctx, host, rc, false )
			}
		}

		for host, record := range cfHosts {
			if _, ok := (*ingressHosts)[host]; !ok {
				klog.InfoS( "Deleting A Record", "name", host )
				cf.deleteRecord( &ctx, record, rc )
			}
		}
	}
}

func ( cf *CloudflareSync ) createRecord( ctx *context.Context, host string, rc *cloudflare.ResourceContainer ) {
	// Why CreateDNSRecordParams wants a pointer?
	proxied := new(bool)
	*proxied = true
	_, err := cf.api.CreateDNSRecord( *ctx, rc, cloudflare.CreateDNSRecordParams{
		Type: "A",
		Proxied: proxied,
		Name: host,
		Content: cf.kube.ExternalIP,
	})
	if err != nil {
		klog.ErrorS( err, "Cannot create DNSRecord", "name", host )
	}
}

func ( cf *CloudflareSync ) deleteRecord( ctx *context.Context, record *cloudflare.DNSRecord, rc *cloudflare.ResourceContainer ) {
	err := cf.api.DeleteDNSRecord( *ctx, rc, (*record).ID)
	if err != nil {
		klog.ErrorS( err, "Cannot delete DNSRecord", "name", record.Name )
	}
}

func ( cf *CloudflareSync ) updateRecord( ctx *context.Context, record *cloudflare.DNSRecord, rc *cloudflare.ResourceContainer ) {
	_, err := cf.api.UpdateDNSRecord( *ctx, rc, cloudflare.UpdateDNSRecordParams{
		ID: record.ID,
		Content: cf.kube.ExternalIP,
	})
	if err != nil {
		klog.ErrorS( err, "Cannot update DNSRecord", "name", record.Name )
	}
}

func ( cf *CloudflareSync ) createOrUpdateHost(
	ctx *context.Context,
	host string, rc *cloudflare.ResourceContainer,
	dryRun bool ) {

	ARecords := cloudflare.ListDNSRecordsParams{ Type: "A", Name: host }
	dnsRecords, _, err := cf.api.ListDNSRecords(*ctx, rc, ARecords)
	if err != nil {
		klog.ErrorS( err, "Cannot get DNSRecords", "name", host )
		return
	}

	if len( dnsRecords ) == 0 {
		klog.InfoS(
			"Creating A Record",
			"dryRun", dryRun,
			"name", host,
			"to", cf.kube.ExternalIP,
		)
		if dryRun {
			return
		}
		cf.createRecord(ctx, host, rc)
		return
	}

	for _, record := range dnsRecords {
		if record.Content != cf.kube.ExternalIP {
			klog.InfoS(
				"Updating A Record",
				"dryRun", dryRun,
				"name", record.Name,
				"from", record.Content,
				"to", cf.kube.ExternalIP,
			)
			if dryRun {
				continue
			}
			cf.updateRecord(ctx, &record, rc)
		} else {
			klog.V(1).InfoS( "Unchanged", "host", record.Name )
		}
	}
}

func ( cf *CloudflareSync ) UpdateHost( host string, dryRun bool ) {

	ctx := context.Background()

	zoneName := getZoneFromHost( host )
	zoneID, err := cf.api.ZoneIDByName( zoneName )
	if err != nil {
		klog.ErrorS( err, "Cannot resolve ZoneID" )
		return
	}

	rc := cloudflare.ZoneIdentifier(zoneID)
	cf.createOrUpdateHost( &ctx, host, rc, dryRun )
}
