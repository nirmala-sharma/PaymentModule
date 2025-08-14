export const environment = {
  production: true,
   // The backend container listens on port 8080 internally (inside Docker), 
  // but in docker-compose, we map host port 5001 -> container port 8080:
  // 
  //   ports:
  //     - "5001:8080"
  //
  // This means requests from outside Docker (like from Angular in the browser) 
  // must target http://localhost:5001/api to reach the backend service.
  apiUrl: 'http://localhost:5001/api',  

  appName: 'Payment App'
};
