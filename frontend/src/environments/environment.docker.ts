// Image Docker : nginx sert le front et relaie /api vers le backend (voir nginx/default.conf.template).
export const environment = {
  name: 'docker',
  production: true,
  apiUrl: '/api',
};
