import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member | undefined;

  constructor(
    private memberService: MembersService,
    private toastrService: ToastrService,
    public presenceService: PresenceService
  ) {}
  ngOnInit(): void {
    console.log(this.member?.gender);
  }

  addLike(member: Member) {
    this.memberService.addLike(member.userName).subscribe({
      next: () => this.toastrService.success('User liked successfully'),
    });
  }
}
