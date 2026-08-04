import { Component, input, output } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss'
})
export class ModalComponent {
  visible = input(false);
  title = input('');
  saving = input(false);
  showFooter = input(true);
  visibleChange = output<boolean>();
  save = output<void>();

  close() { this.visibleChange.emit(false); }
}
