FROM nginx

COPY proxy.conf /etc/nginx/templates/default.conf.template
COPY proxy-routes.conf /etc/nginx/templates/routes/default.conf.template