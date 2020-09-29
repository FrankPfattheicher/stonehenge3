
openssl req -newkey rsa:2048 -nodes -keyout stoneKey.key -x509 -days 365 -out stonehenge.pem
openssl pkcs12 -inkey stoneKey.pem -in stonehenge.pem -export -out stonehenge.p12
openssl pkcs12 -export -out stonehenge.pfx -inkey stoneKey.key -in stonehenge.pem
