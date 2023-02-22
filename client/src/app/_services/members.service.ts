import { HttpClient } from '@angular/common/http';
import { Injectable, Query } from '@angular/core';
import { map, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { QueryParams } from '../_models/queryParams';
import { User } from '../_models/user';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User | undefined;
  queryParams: QueryParams | undefined;

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    //call the service and set the current user and get query params
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.queryParams = new QueryParams(user);
          this.user = user;
        }
      },
    });
  }

  getQueryParams() {
    return this.queryParams;
  }

  setQueryParams(params: QueryParams) {
    this.queryParams = params;
  }

  resetQueryParams() {
    console.log(this.user);
    if (this.user) {
      this.queryParams = new QueryParams(this.user);
      return this.queryParams;
    }
    return;
  }

  getMembers(queryParams: QueryParams) {
    const response = this.memberCache.get(Object.values(queryParams).join('-'));

    if (response) return of(response);

    let params = getPaginationHeaders(
      queryParams.pageNumber,
      queryParams.pageSize
    );

    params = params.append('minAge', queryParams.minAge);
    params = params.append('maxAge', queryParams.maxAge);
    params = params.append('gender', queryParams.gender);
    params = params.append('orderBy', queryParams.orderBy);

    // if (this.members.length > 0) {
    //   return of(this.members);
    // }
    return getPaginatedResult<Member[]>(
      this.baseUrl + 'users',
      params,
      this.http
    ).pipe(
      map((response) => {
        this.memberCache.set(Object.values(queryParams).join('-'), response);
        return response;
      })
    );
  }

  getMember(username: string) {
    if (this.memberCache.size > 0) {
      const member = [...this.memberCache.values()]
        .reduce((arr, elem) => arr.concat(elem.result), [])
        .find((member: Member) => member.userName === username);

      if (member) return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }

  uploadImage(imageUrl: string) {
    return this.http.post(this.baseUrl + 'users/user-photo', {
      imageUrl: imageUrl,
    });
  }

  setImageAsMain(photoId: number) {
    return this.http.put(this.baseUrl + `users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + `users/delete-photo/${photoId}`);
  }

  addLike(username: string) {
    return this.http.post(this.baseUrl + 'likes/' + username, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);

    return getPaginatedResult<Member[]>(
      this.baseUrl + 'likes',
      params,
      this.http
    );
  }
}
