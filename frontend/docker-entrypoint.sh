#!/bin/sh
# Lets us change the backend URL without rebuilding the image.
cat > /usr/share/nginx/html/config.js <<CONFIG
window.APP_CONFIG = { apiUrl: "${API_URL}" };
CONFIG
echo "Frontend will call the API at ${API_URL}"
