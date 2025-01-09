import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const containsUppercase = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const hasUpperCase = /[A-Z]/.test(value);
  return hasUpperCase ? null : { containsUppercase: true };
};
