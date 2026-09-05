import { Component, input, output } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-side-drawer',
  standalone: true,
  imports: [LucideDynamicIcon],
  templateUrl: './side-drawer.component.html',
  styleUrl: './side-drawer.component.scss'
})
export class SideDrawerComponent {
  visible = input(false);
  title = input('');
  icon = input('');
  showFooter = input(false);
  saving = input(false);
  visibleChange = output<boolean>();
  save = output<void>();

  close() { this.visibleChange.emit(false); }
}