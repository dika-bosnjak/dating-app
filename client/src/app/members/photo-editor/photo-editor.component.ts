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
    //get the current user from the service
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
  }

  //on init, set the apikey from env
  ngOnInit(): void {
    this.apikey = environment.filestackApiKey;
  }

  //on success, call the service and show the message, add the image in member images
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
          //if the new image is set as main, set the user image again, set the member image
          if (response.isMain && this.user && this.member) {
            this.user.photoUrl = response.url;
            this.accountService.setCurrentUser(this.user);

            this.member.photoUrl = response.url;
          }
        },
      });
    }
  }

  //on error during image upload, display the message
  onError(err: any) {
    console.log('###uploadError', err);
    this.toastrService.error('Image could not be uploaded.');
  }

  //setMainPhoto --> call the service, if the image is set as main
  setMainPhoto(photo: Photo) {
    this.memberService.setImageAsMain(photo.id).subscribe({
      next: (_) => {
        //if it is a user and a member
        if (this.user && this.member) {
          //set user photo, new user main photo
          this.user.photoUrl = photo.url;
          //set the current user in service again, so the new image can be loaded
          this.accountService.setCurrentUser(this.user);
          //set the member photo
          this.member.photoUrl = photo.url;
          //loop through the user's photos and set the isMain property
          this.member.photos.forEach((p) => {
            if (p.isMain) p.isMain = false;
            if (p.id == photo.id) p.isMain = true;
          });
        }
        //display the message
        this.toastrService.success('Image is set as main successfully.');
      },
    });
  }

  //deletePhoto --> call the service, if the image is deleted, filter the member photos and show the message
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
