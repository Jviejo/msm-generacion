server {
    root /var/www/msm.tributec.es;
    index index.html index.htm index.nginx-debian.html;
    listen 443 ssl;
    server_name msm.rinconeducativo.com;

    location / {
        try_files $uri $uri/ =404;
    }

    # Certbot well-known location for ACME challenges
    location ~ /.well-known/acme-challenge {
        allow all;
        root /var/www/certbot;
    }

    listen [::]:443 ssl; # managed by Certbot
    ssl_certificate /etc/letsencrypt/live/msm.rinconeducativo.com/fullchain.pem; # managed by Certbot
    ssl_certificate_key /etc/letsencrypt/live/msm.rinconeducativo.com/privkey.pem; # managed by Certbot
    include /etc/letsencrypt/options-ssl-nginx.conf; # managed by Certbot
    ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem; # managed by Certbot

}
server {
    listen 80;
    listen [::]:80;
    server_name msm.rinconeducativo.com;

    # This block is for Certbot's challenge verification
    location ~ /.well-known/acme-challenge {
        allow all;
        root /var/www/certbot;
    }

    # Redirect all HTTP traffic to HTTPS
    return 301 https://$host$request_uri;
}