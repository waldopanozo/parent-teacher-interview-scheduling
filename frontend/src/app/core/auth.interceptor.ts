import { HttpInterceptorFn } from '@angular/common/http';

const tokenKey = 'pta_access_token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(tokenKey);
  if (token && !req.headers.has('Authorization')) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};

export { tokenKey };
