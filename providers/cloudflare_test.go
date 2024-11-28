package providers

import (
    "testing"
)

func TestAll(t *testing.T) {
	c := CloudflareSync{}
	err := c.Initialize()
	if err != nil {
		t.Error( err )
	}
	c.ExternalIP = "127.0.0.1"
	c.UpdateHost( "demo.example.com" )
}
