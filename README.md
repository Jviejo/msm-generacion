# 

Clonar el repositorio
```
git clone https://github.com/Jviejo/msm-generacion
```

### Crear una ssh key conn ecc
```
ssh-keygen -t ed25519 -C "msm@rinconeducativo.es" -f ~/.ssh/msm-key
```

Transferir la clave pública al host
```
ssh-copy-id -i ~/.ssh/msm-key.pub root@217.72.207.5
```


Instalar .NET 9.0
```
brew install --cask dotnet-sdk
```

Compilar la aplicación
```
dotnet build
```

Ejecutar la aplicación
```
dotnet run
```

Instalar rsync
```
brew install rsync
brew upgrade rsync
```

Subir la aplicación a la máquina virtual.
```
dotnet build && dotnet run && /usr/local/bin/rsync -avP --chown=www-data:www-data --iconv=. -e "ssh -i ~/.ssh/msm-key" ./generado/* root@217.72.207.5:/var/www/msm.tributec.es/
```

para validar el certificado ssl
https://www.ssllabs.com/ssltest/analyze.html?d=www.rinconeducativo.com