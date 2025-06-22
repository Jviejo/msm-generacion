# 
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

Subir la aplicación a la máquina virtual.
```
rsync -avP --iconv=. -e "ssh -i ~/.ssh/ec2-jvh2025-key" ./generado/* root@217.72.207.5:/var/www/msm.tributec.es/
```

para validar el certificado ssl
https://www.ssllabs.com/ssltest/analyze.html?d=msm.rinconeducativo.com