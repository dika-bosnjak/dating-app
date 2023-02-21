import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};
  user: User | null = null;
  queryParams: any;

  constructor(public accountService: AccountService, private router: Router) {}

  ngOnInit(): void {}

  //on login
  login() {
    //login, if the user is successfully logged in, navigate to the members page
    this.accountService.login(this.model).subscribe({
      next: (_) => {
        this.router.navigateByUrl('/members');
      },
    });
  }

  //on logout
  logout() {
    //remove the user from localstorage
    this.accountService.logout();
    //navigate to the home page
    this.router.navigateByUrl('/');
  }
}
