# # Stage 1: Build the Angular app
# FROM node:16.16.0-alpine AS builder

# # Set working directory
# WORKDIR /app

# # Copy package files and install dependencies
# COPY package*.json ./
# RUN npm install

# # Copy the rest of the app source code
# COPY . .

# # Build the Angular app (using correct build script)
# RUN npm run build

# # Stage 2: Serve the app using Nginx
# FROM nginx:alpine

# # Replace the default Nginx website with your Angular app
# COPY --from=builder /app/dist/paymentgatewayapp.client /usr/share/nginx/html

# # Expose port 80
# EXPOSE 80

# # Start Nginx in the foreground
# CMD ["nginx", "-g", "daemon off;"]
# Stage 1: Build the Angular app
FROM node:16.16.0-alpine AS builder

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build -- --configuration=production

# Stage 2: Serve the app using Nginx
FROM nginx:alpine

# Copy build output
COPY --from=builder /app/dist/paymentgatewayapp.client /usr/share/nginx/html

# 🔥 Copy your custom nginx.conf
COPY default.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
