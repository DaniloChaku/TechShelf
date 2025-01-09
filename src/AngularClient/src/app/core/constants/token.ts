import { HttpContextToken } from '@angular/common/http';

export const TOKEN = 'token';
export const IS_REFRESH_TOKEN = new HttpContextToken(
  () => false
);
