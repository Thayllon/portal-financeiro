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

  private iniciouNoOverlay = false;

  close() { this.visibleChange.emit(false); }

  aoMouseDownNoOverlay(event: MouseEvent) {
    this.iniciouNoOverlay = event.target === event.currentTarget;
  }

  aoMouseUpNoOverlay() {
    if (this.iniciouNoOverlay) {
      this.close();
    }
    this.iniciouNoOverlay = false;
  }
}
