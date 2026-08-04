import { Component, input, output, signal, effect, ElementRef, HostListener, forwardRef } from '@angular/core';
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
export class CustomSelectComponent implements ControlValueAccessor {
  label = input('');
  placeholder = input('Selecione...');
  options = input<SelectOption[]>([]);
  value = input('');
  valueChange = output<string>();

  private _disabled = signal(false);
  disabled = this._disabled.asReadonly();

  internalValue = signal('');
  isOpen = signal(false);
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
      this.isOpen.set(false);
    }
  }

  toggle() {
    if (!this.disabled()) {
      this.isOpen.update(v => !v);
    }
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
