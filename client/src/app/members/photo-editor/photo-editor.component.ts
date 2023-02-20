import { IMAGE_CONFIG } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css'],
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member | undefined;

  apikey: string | undefined;
  baseUrl = environment.apiUrl;
  user: User | undefined;

  constructor(
    private accountService: AccountService,
    private toastrService: ToastrService,
    private memberService: MembersService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
  }

  ngOnInit(): void {
    this.apikey = environment.filestackApiKey;
  }

  onSuccess(res: any) {
    if (this.user && res.filesUploaded[0].url) {
      this.memberService.uploadImage(res.filesUploaded[0].url).subscribe({
        next: (response: any) => {
          this.toastrService.success('Image uploaded successfully.');
          this.member?.photos.push({
            id: response.id,
            url: response.url,
            isMain: response.isMain,
          });
        },
      });
    }
  }

  onError(err: any) {
    console.log('###uploadError', err);
    this.toastrService.error('Image could not be uploaded.');
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setImageAsMain(photo.id).subscribe({
      next: (_) => {
        if (this.user && this.member) {
          this.user.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
          this.member.photoUrl = photo.url;
          this.member.photos.forEach((p) => {
            if (p.isMain) p.isMain = false;
            if (p.id == photo.id) p.isMain = true;
          });
        }
        this.toastrService.success('Image is set as main successfully.');
      },
    });
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo.id).subscribe({
      next: (_) => {
        if (this.member) {
          this.member.photos = this.member.photos.filter(
            (image) => image.id !== photo.id
          );
          this.toastrService.success('Image is deleted successfully.');
        }
      },
    });
  }
}
