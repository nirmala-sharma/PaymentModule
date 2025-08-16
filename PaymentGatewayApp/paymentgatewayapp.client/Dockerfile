# Dockerfile to build and serve the Angular app using Nginx

# Stage 1: Build the Angular app
FROM node:16.16.0-alpine AS builder
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm install

# Copy the rest of the project and build for production
COPY . .
RUN npm run build -- --configuration=production

# Stage 2: Serve the Angular app using Nginx
FROM nginx:alpine

# Copy the built Angular app from the builder stage
COPY --from=builder /app/dist/paymentgatewayapp.client /usr/share/nginx/html

# Copy custom Nginx config to use default.conf
# This config handles serving Angular and forwarding API requests to backend
COPY default.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for HTTP access
EXPOSE 80

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]

