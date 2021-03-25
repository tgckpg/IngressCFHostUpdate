
## Install
```
helm install [MY_INSTALLATION_NAME] helm.chart -n [MY_NAMESPACE] --set providers.cloudFlare.apiToken=[CLOUD_FLARE_API_TOKEN]
```

## How it works
This controller will update the DNS record by using the API provided from the DNS provider. It'll try to update the record when:
* An ingress resources has been created ( `kubectl apply -f ingress-conf.yaml` )
* A weekly cron job is triggered to sync weekly for deleted records


#### Generated DNS Record
It creates an `A` record that points to the LoadBalancer's external IP address. ( It'll use the first one if you have multiple LoadBalancer within the same cluster. )


## Supported Providers

### CloudFlare
An API Token is required for updating DNS Records. The following permissions are required for this to work. You can use the following link to create one after logging in:
* https://dash.cloudflare.com/profile/api-tokens

#### Required permissions
* All zones - **Zone:Read, DNS:Edit**
