# List
curl https://localhost:7224/products -k

# Details
curl https://localhost:7224/products/1 -k

# Create (missing admin header -> 403)
curl -s -X POST https://localhost:7224/products -H "Content-Type: application/json" -d '{"name":"Boots Alpha","basePrice":1299}' -k -i

# Create (authorized)
curl -s -X POST https://localhost:7224/products -H "X-Admin: true" -H "Content-Type: application/json" -d '{"name":"Boots Alpha","basePrice":1299, "slug": "boots-alpha"}' -k -i

# Update
curl -s -X PUT https://localhost:7224/products/1 -H "X-Admin: true" -H "Content-Type: application/json" -d '{"id":1,"name":"Team Jersey (2025)","basePrice":1099,"isActive":true}' -k -i

# Delete
curl -s -X DELETE https://localhost:7224/products/2 -H "X-Admin: true" -k -i

curl -i https://localhost:7224/__endpoints -k