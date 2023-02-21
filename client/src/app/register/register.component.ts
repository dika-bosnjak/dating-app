import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  registerForm: FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  //on init
  ngOnInit(): void {
    //initialize register form
    this.initializeForm();
    //set the max date (user must be older than 18)
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  //iinitialize the registration form
  initializeForm() {
    //create form builder group with default values and validators
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      gender: ['male'],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: [
        '',
        [Validators.required, Validators.minLength(4), Validators.maxLength(8)],
      ],
      confirmPassword: [
        '',
        [Validators.required, this.matchValues('password')],
      ],
    });
    //value change checker - validate on password change
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
  }

  //matchValues is used for form validation, check whether the confirm password matches the password
  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value == control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }

  //register function
  register() {
    //get the date from the dateOfBirth field (without time - timezone)
    const dob = this.getDateOnly(
      this.registerForm.controls['dateOfBirth'].value
    );
    //save the values from the form in the object
    const values = { ...this.registerForm.value, dateOfBirth: dob };

    //use account service to register the user
    this.accountService.register(values).subscribe({
      //if the user is registered successfully, display members
      next: (_) => {
        this.router.navigateByUrl('/members');
      },
      //if there are some errors, display it as validation errors
      error: (error) => {
        this.validationErrors = error;
      },
    });
  }

  //cancel the registration process, navigate to home page
  cancel() {
    this.router.navigateByUrl('/');
  }

  //get the date from the date of birth
  private getDateOnly(dob: string | undefined) {
    if (!dob) return;
    let theDob = new Date(dob);
    return new Date(
      theDob.setMinutes(theDob.getMinutes() - theDob.getTimezoneOffset())
    )
      .toISOString()
      .slice(0, 10);
  }
}
