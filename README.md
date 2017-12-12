# bsdmn-api
HTTP JSON-RPC API for retrieving BitSend masternode info.

The bsdmn api implements the JSON-RPC 2.0 spec over HTTP. It accepts POST with a JSON request in the body.

# Sample using curl
**List masternodes**

`curl -X POST -H "Content-Type: application/json" -d "{\"method\": \"masternode.list\", \"params\": {\"status\": \"ENABLED\"}}" http://api.bsdmn.info/`

Response:

```json
{
  "jsonrpc": "2.0",
  "result": [
    {
      "id": "45.76.8.98:8886-0",
      "address": "45.76.8.98:8886",
      "status": "ENABLED",
      "protocol": 70082,
      "pubKey": "iQR1kdWVNyR8V3ZuMWV8SMJXZAqsnTd9Bf",
      "vin": "69c93f13cc543ea9e5c8bc37fc830887b049df7c13d3847972c72f034fbc5a85",
      "lastSeen": "2017-12-10T23:45:12",
      "activeDuration": "64.02:27:28",
      "rank": 50
    },
    {
      "id": "185.194.142.125:8886-7",
      "address": "185.194.142.125:8886",
      "status": "ENABLED",
      "protocol": 70082,
      "pubKey": "iH4kUoxwLawgAiSCZAQN9sQMhgWKkSGaCw",
      "vin": "d393983890b096efb5cd6a5ccdb7042e29ca25c6ae55902ec60b4a5c5dec53c8",
      "lastSeen": "2017-12-11T00:10:28",
      "activeDuration": "40.03:01:44",
      "rank": 121
    }
```

Available params: "status", "protocol"

**Get specific masternode**

`curl -X POST -H "Content-Type: application/json" -d "{\"method\": \"masternode.get\", \"params\": {\"address\": \"66.70.142.214\"}}" http://api.bsdmn.info/`

Response:

```json
{
  "jsonrpc": "2.0",
  "result": {
    "id": "66.70.142.214:8886-0",
    "address": "66.70.142.214:8886",
    "status": "ENABLED",
    "protocol": 70082,
    "pubKey": "iAuxuZzM7j6LwoVhfyLkJ1pSM9SYMpUz3W",
    "vin": "28d34db029ef7b45027eed046c7ac51820d994fef090a05ce1a43ad3ca35f995",
    "lastSeen": "2017-12-10T23:53:08",
    "activeDuration": "6.02:02:02",
    "rank": 337
  }
}
```

Available params: "address", "vin", "pubkey", "nodeId"

**Get masternode count**

`curl -X POST -H "Content-Type: application/json" -d "{\"method\": \"masternode.getcount\", \"params\": {}}" http://api.bsdmn.info/`

Response:

```json
{
  "jsonrpc": "2.0",
  "result": 369
}
```

Available params: "status", "protocol"

**Get donate address :)**

`curl -X POST -H "Content-Type: application/json" -d "{\"method\": \"wallet.getdonateaddress\", \"params\": {}}" http://api.bsdmn.info/`

Response:

```json
{
  "jsonrpc": "2.0",
  "result": "i6RE62Sp3khLcTdCjC7Pji78x2c5PmbpDG"
}
```
