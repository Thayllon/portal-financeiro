import { Component, inject } from '@angular/core';
import { ConfirmService } from '../services/confirm.service';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  confirm = inject(ConfirmService);
}
