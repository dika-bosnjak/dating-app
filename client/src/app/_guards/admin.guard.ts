import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  /**
   *
   */
  constructor(
    private accountService: AccountService,
    private toastrService: ToastrService
  ) {}
  canActivate(): Observable<boolean> {
    return this.accountService.currentUser$.pipe(
      map((user) => {
        if (!user) return false;
        if (user.roles.includes('Admin') || user.roles.includes('Moderator')) {
          return true;
        } else {
          this.toastrService.error('You are not allowed to access this.');
          return false;
        }
      })
    );
  }
}
