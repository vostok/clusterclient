## Well-known ClusterClient headers

| Name  | Origin | Example value | Description |
| ----- | ------ | ------------- | ----------- |
| `Dont-Retry`           | Response | `true`                     | The client accepts any response with this header, even one indicating an obvious failure. |
| `Application-Identity` | Request  | `SampleApp`                | Request issuer's application name. Commonly set to process name by default. |
| `Request-Timeout`      | Request  | `23.908`                  | Request timeout on client side, expressed in seconds in following format: `0.###` |
| `Request-Priority`     | Request  | `Ordinary`                 | Request priority. May have one of three values: `Sheddable`, `Ordinary`, `Critical`. |
| `Context-Properties`   | Request  | `BAAAAHByb3ADAAAAMTIz`     | Serialized ambient context properties in Base64. Opaque to non-Vostok applications. |
| `Context-Globals`      | Request  | `BgAAAGdsb2JhbAMAAAAxMjM=` | Serialized ambient context globals in Base64. Opaque to non-Vostok applications. |
