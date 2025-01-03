import {
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';

export const phoneNumber = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  const isValidPhoneNumber = /^\+[0-9\-]{9,15}$/.test(
    value
  );
  return isValidPhoneNumber ? null : { phoneNumber: true };
};
