import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  //set the title
  title = 'Dating App';
  //constructor - use accountService
  constructor(private accountService: AccountService) {}

  //on init, set the current user
  ngOnInit(): void {
    this.setCurrentUser();
  }

  //get the user data from the localstorage and set the current user in the account service
  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user: User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }
}
