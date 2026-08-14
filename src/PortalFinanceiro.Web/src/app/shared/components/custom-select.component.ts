import { Component, input, output, signal, effect, ElementRef, HostListener, forwardRef, OnDestroy } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { LucideDynamicIcon } from '@lucide/angular';

export interface SelectOption {
  value: string;
  label: string;
  icon?: string;
}

@Component({
  selector: 'app-custom-select',
  standalone: true,
  imports: [LucideDynamicIcon],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CustomSelectComponent),
      multi: true,
    },
  ],
  templateUrl: './custom-select.component.html',
  styleUrl: './custom-select.component.scss',
})
export class CustomSelectComponent implements ControlValueAccessor, OnDestroy {
  label = input('');
  placeholder = input('Selecione...');
  options = input<SelectOption[]>([]);
  value = input('');
  valueChange = output<string>();

  private _disabled = signal(false);
  disabled = this._disabled.asReadonly();

  internalValue = signal('');
  isOpen = signal(false);
  openUp = signal(false);
  selectedLabel = signal('');

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private el: ElementRef) {
    effect(() => {
      const val = this.value();
      if (val !== undefined) {
        this.internalValue.set(val);
      }
    });

    effect(() => {
      const val = this.internalValue();
      const opts = this.options();
      const found = opts.find(o => o.value === val);
      this.selectedLabel.set(found?.label ?? '');
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    if (!this.el.nativeElement.contains(event.target)) {
      this.fechar();
    }
  }

  @HostListener('window:resize')
  onResize() {
    if (this.isOpen()) this.posicionarDropdown();
  }

  private scrollHandler = () => {
    if (this.isOpen()) this.posicionarDropdown();
  };

  toggle() {
    if (!this.disabled()) {
      const willOpen = !this.isOpen();
      this.isOpen.set(willOpen);
      if (willOpen) {
        setTimeout(() => {
          this.posicionarDropdown();
          document.addEventListener('scroll', this.scrollHandler, true);
        });
      } else {
        this.fechar();
      }
    }
  }

  private fechar() {
    this.isOpen.set(false);
    document.removeEventListener('scroll', this.scrollHandler, true);
  }

  ngOnDestroy() {
    document.removeEventListener('scroll', this.scrollHandler, true);
  }

  private posicionarDropdown() {
    const trigger = this.el.nativeElement.querySelector('.cs__trigger') as HTMLElement;
    const dropdown = this.el.nativeElement.querySelector('.cs__dropdown') as HTMLElement | null;
    if (!trigger || !dropdown) return;

    const rect = trigger.getBoundingClientRect();
    const alturaDropdown = dropdown.offsetHeight || 240;
    const espacoAbaixo = window.innerHeight - rect.bottom;
    const espacoAcima = rect.top;
    const abrirParaCima = espacoAbaixo < alturaDropdown && espacoAcima > espacoAbaixo;
    this.openUp.set(abrirParaCima);

    dropdown.style.position = 'fixed';
    dropdown.style.left = `${rect.left}px`;
    dropdown.style.width = `${rect.width}px`;
    dropdown.style.zIndex = '10000';
    dropdown.style.top = abrirParaCima ? `${rect.top - alturaDropdown - 4}px` : `${rect.bottom + 4}px`;
    dropdown.style.bottom = 'auto';
  }

  select(option: SelectOption) {
    this.internalValue.set(option.value);
    this.selectedLabel.set(option.label);
    this.isOpen.set(false);
    this.onChange(option.value);
    this.onTouched();
    this.valueChange.emit(option.value);
  }

  writeValue(value: string): void {
    this.internalValue.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this._disabled.set(isDisabled);
  }
}
