#!/bin/bash

# Nombre del dominio
DOMAIN="msm.rinconeducativo.com"
WWW_DOMAIN="msm.rinconeducativo.com" # Opcional, si también quieres certificar el subdominio www

# Email para notificaciones de Let's Encrypt
EMAIL="jviejo@gmail.com" # ¡IMPORTANTE! Reemplaza con tu dirección de correo electrónico

echo "--- Script de instalación de SSL para Nginx con Certbot ---"
echo "Dominio: $DOMAIN"
echo "Email de contacto para Let's Encrypt: $EMAIL"
echo ""

# Paso 1: Actualizar el sistema e instalar Certbot
echo "Paso 1: Actualizando el sistema e instalando Certbot..."
# # sudo apt update -y
# # sudo apt install snapd -y
# # sudo snap install core; sudo snap refresh core
# # sudo snap install --classic certbot
# # sudo ln -s /snap/bin/certbot /usr/bin/certbot

# if [ $? -ne 0 ]; then
#     echo "ERROR: Fallo al instalar Certbot. Abortando."
#     exit 1
# fi
# echo "Certbot instalado correctamente."
# echo ""

# Paso 2: Asegurarse de que Nginx está configurado para el dominio
echo "Paso 2: Verificando la configuración de Nginx para $DOMAIN..."
NGINX_CONF="/etc/nginx/sites-available/$DOMAIN"

if [ ! -f "$NGINX_CONF" ]; then
    echo "Creando una configuración básica de Nginx para $DOMAIN..."
    sudo bash -c "cat > $NGINX_CONF <<EOF
server {
    listen 80;
    listen [::]:80;
    server_name $DOMAIN;

    root /var/www/$DOMAIN; # Asegúrate de que este directorio exista y tenga contenido
    index index.html index.htm;

    location / {
        try_files \$uri \$uri/ =404;
    }
}
EOF"
    echo "Configuración básica creada en $NGINX_CONF"
    sudo ln -s $NGINX_CONF /etc/nginx/sites-enabled/
    echo "Enlace simbólico creado en /etc/nginx/sites-enabled/"
else
    echo "El archivo de configuración de Nginx '$NGINX_CONF' ya existe. Asegúrate de que contiene 'server_name $DOMAIN $WWW_DOMAIN;' y escucha en el puerto 80."
fi

# Probar la configuración de Nginx y recargar
echo "Probando la configuración de Nginx..."
sudo nginx -t
if [ $? -ne 0 ]; then
    echo "ERROR: La configuración de Nginx tiene errores. Por favor, revisa $NGINX_CONF. Abortando."
    exit 1
fi
echo "Configuración de Nginx correcta. Recargando Nginx."
sudo systemctl reload nginx
if [ $? -ne 0 ]; then
    echo "ERROR: Fallo al recargar Nginx. Abortando."
    exit 1
fi
echo "Nginx recargado."
echo ""

# Paso 3: Configurar el firewall (UFW) si está activo
echo "Paso 3: Configurando el firewall (UFW) para permitir HTTP y HTTPS..."
if sudo ufw status | grep -q "Status: active"; then
    echo "Firewall UFW activo. Abriendo puertos 80 (Nginx HTTP) y 443 (Nginx Full)."
    sudo ufw allow 'Nginx HTTP'
    sudo ufw allow 'Nginx Full'
    sudo ufw reload
    echo "Puertos 80 y 443 abiertos en UFW."
else
    echo "Firewall UFW no activo o no detectado. Si usas otro firewall, asegúrate de que los puertos 80 y 443 están abiertos."
fi
echo ""

# Paso 4: Obtener e instalar el certificado SSL con Certbot
echo "Paso 4: Obteniendo e instalando el certificado SSL para $DOMAIN con Certbot..."
# Usamos --nginx para que Certbot modifique automáticamente la configuración de Nginx
# y configure la redirección de HTTP a HTTPS.
sudo certbot --nginx -d $DOMAIN -d $WWW_DOMAIN --email $EMAIL --agree-tos --no-eff-email

if [ $? -ne 0 ]; then
    echo "ERROR: Fallo al obtener/instalar el certificado SSL. Revisa los logs de Certbot."
    echo "Puedes encontrar logs en /var/log/letsencrypt/"
    exit 1
fi
echo "Certificado SSL obtenido e instalado correctamente para $DOMAIN."
echo ""

# Paso 5: Probar la renovación automática (dry run)
echo "Paso 5: Probando la renovación automática del certificado..."
sudo certbot renew --dry-run
if [ $? -ne 0 ]; then
    echo "ADVERTENCIA: La prueba de renovación automática falló. Esto podría indicar un problema con futuras renovaciones."
    echo "Revisa la salida anterior para depurar el problema."
else
    echo "La prueba de renovación automática fue exitosa. La renovación se configurará automáticamente."
fi
echo ""

echo "--- ¡Configuración SSL completada! ---"
echo "Visita https://$DOMAIN en tu navegador para verificar el certificado."
echo "Puedes usar herramientas como https://www.ssllabs.com/ssltest/ para un análisis más profundo."