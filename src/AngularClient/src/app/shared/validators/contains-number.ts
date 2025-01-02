import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const containsNumber = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const hasNumber = /[0-9]/.test(value);
  return hasNumber ? null : { containsNumber: true };
};
