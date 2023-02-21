import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { QueryParams } from 'src/app/_models/queryParams';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent implements OnInit {
  members: Member[] | undefined;
  pagination: Pagination | undefined;
  queryParams: QueryParams | undefined;
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  constructor(private memberService: MembersService) {
    this.queryParams = this.memberService.getQueryParams();
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  //on load members
  loadMembers() {
    //if there is no query params, return
    if (this.queryParams) {
      this.memberService.setQueryParams(this.queryParams);
      //call the service and set the members and pagination
      this.memberService.getMembers(this.queryParams).subscribe({
        next: (response) => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        },
      });
    }
  }

  //on reset filters, set the default values
  resetFilters() {
    this.queryParams = this.memberService.resetQueryParams();
    this.loadMembers();
  }

  //on page change (pagination), set the page number and load members
  pageChanged(event: any) {
    if (this.queryParams && this.queryParams?.pageNumber !== event.page) {
      this.queryParams.pageNumber = event.page;
      this.memberService.setQueryParams(this.queryParams);
      this.loadMembers();
    }
  }
}
