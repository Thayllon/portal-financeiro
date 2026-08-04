import { Component, inject } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss'
})
export class ToastComponent {
  notification = inject(NotificationService);
}
