FROM golang:1.23-alpine AS build

WORKDIR /app

COPY [ "go.mod", "go.sum", "./" ]

RUN go mod download

COPY "controllers" "./controllers/"
COPY "providers" "./providers/"
COPY *.go ./

RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -ldflags="-w -s" -o /ingress-dns-sync

FROM scratch

COPY --from=alpine:latest /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY --from=build /ingress-dns-sync /

WORKDIR /
CMD [ "/ingress-dns-sync", "--v=1" ]
