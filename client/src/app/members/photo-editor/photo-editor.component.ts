import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
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
    private memberService: MembersService,
    private http: HttpClient
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

  async onSuccess(res: any) {
    for (let uploadedImage of res.filesUploaded) {
      if (this.user && uploadedImage) {
        await this.http.post(this.baseUrl + 'users/user-photo', {
          imageUrl: uploadedImage.url,
        });
        var response = this.memberService.uploadImage(uploadedImage.url);
        console.log('###uploadSuccess', response);
      }
    }
  }

  onError(err: any) {
    console.log('###uploadError', err);
    this.toastrService.error('Image could not be uploaded.');
  }
}

/*
{
    "filesUploaded": [
        {
            "filename": "IMG_7861.jpg",
            "handle": "pAIahoOiQVeccIP5fgwO",
            "mimetype": "image/jpeg",
            "originalPath": "IMG_7861.jpg",
            "size": 180403,
            "source": "local_file_system",
            "url": "https://cdn.filestackcontent.com/pAIahoOiQVeccIP5fgwO",
            "uploadId": "K2jFJ3R1pO4u99UO",
            "originalFile": {
                "name": "IMG_7861.jpg",
                "type": "image/jpeg",
                "size": 180403
            },
            "status": "Stored"
        }
    ],
    "filesFailed": []
} */
