# Install bsdmn API on Ubuntu 16.04

```
sudo apt-get update
```

## Install .NET Core SDK
```
https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/sdk-current
```

## Clone and build bsdmn-api
```
git clone https://github.com/shawnshaddock/bsdmn-api.git
dotnet build -c "Release" bsdmn-api/src
sudo cp -r bsdmn-api/src/bsdmn.Api/bin/Release/netcoreapp2.0 /var/bsdmn-api
```

## Create bsdmn-api service
```
sudo nano /etc/systemd/system/bsdmn-api.service
```

```
[Unit]
Description=bsdmn API

[Service]
WorkingDirectory=/var/bsdmn-api
ExecStart=/usr/bin/dotnet /var/bsdmn-api/bsdmn.Api.dll
Restart=always
RestartSec=10
SyslogIdentifier=bsdmn-api
User=root
Environment=APP_ENV=Testing

[Install]
WantedBy=multi-user.target
```

```
CTRL+X
Y
ENTER
```

```
sudo systemctl enable bsdmn-api.service
sudo systemctl start bsdmn-api.service
```

## Check service status
```
sudo systemctl status bsdmn-api.service
sudo journalctl -fu bsdmn-api.service
```