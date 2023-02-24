import {
  AfterViewChecked,
  Component,
  ElementRef,
  Input,
  OnInit,
  ViewChild,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
})
export class MemberMessagesComponent implements OnInit, AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('chatWindow')
  private myScrollContainer!: ElementRef;
  @Input() username?: string;
  messageContent = '';

  constructor(
    public messageService: MessageService,
    private toastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this.scrollToBottom();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop =
        this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) {}
  }

  sendMessage() {
    if (!this.username) return;
    this.messageService
      .sendMessage(this.username, this.messageContent)
      .then(() => {
        this.messageForm?.reset();
      });
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        // this.messages = this.messages?.filter((m) => m.id != id);
        // this.toastrService.success('Message is deleted successfully.');
      },
    });
  }
}
