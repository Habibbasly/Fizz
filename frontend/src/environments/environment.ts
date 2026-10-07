// Environnement de développement (ng serve / npm start).
// Remplacé au build par environment.preprod.ts ou environment.prod.ts (voir angular.json → fileReplacements).
export const environment = {
  name: 'development',
  production: false,
  // Relatif : le proxy (proxy.conf.json) redirige /api vers le backend .NET local.
  apiUrl: '/api',
};
