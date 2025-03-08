import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const phoneNumber = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const isValidPhoneNumber = /^\+1[0-9]{10}$/.test(value);
  return isValidPhoneNumber ? null : { phoneNumber: true };
};
