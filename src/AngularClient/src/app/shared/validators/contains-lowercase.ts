import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const containsLowercase = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const hasLowerCase = /[a-z]/.test(value);
  return hasLowerCase ? null : { containsLowercase: true };
};
