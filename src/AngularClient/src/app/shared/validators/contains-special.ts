import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const containsSpecial = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(value);
  return hasSpecial ? null : { containsSpecial: true };
};
