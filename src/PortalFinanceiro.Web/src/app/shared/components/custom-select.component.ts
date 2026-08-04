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
  template: `
    <div class="cs" [class.cs--open]="isOpen()" [class.cs--disabled]="disabled()">
      @if (label()) {
        <label class="cs__label">{{ label() }}</label>
      }
      <button
        type="button"
        class="cs__trigger"
        [class.cs__trigger--placeholder]="!selectedLabel()"
        (click)="toggle()"
        [attr.aria-expanded]="isOpen()"
        [attr.aria-haspopup]="'listbox'"
        [disabled]="disabled()"
      >
        <span class="cs__trigger-text">{{ selectedLabel() || placeholder() }}</span>
        <svg lucideIcon="chevron-down" class="cs__chevron" [size]="16" [class.cs__chevron--open]="isOpen()" />
      </button>
      @if (isOpen()) {
        <div class="cs__dropdown" role="listbox">
          @for (opt of options(); track opt.value) {
            <button
              type="button"
              class="cs__option"
              [class.cs__option--selected]="opt.value === internalValue()"
              (click)="select(opt)"
              [attr.role]="'option'"
              [attr.aria-selected]="opt.value === internalValue()"
            >
              {{ opt.label }}
            </button>
          }
          @if (options().length === 0) {
            <div class="cs__empty">Nenhuma opção</div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .cs { position: relative; width: 100%; }
    .cs__label {
      display: block;
      font-size: 0.8125rem;
      font-weight: 500;
      color: var(--text-secondary);
      margin-bottom: 0.375rem;
    }
    .cs__trigger {
      display: flex;
      align-items: center;
      justify-content: space-between;
      width: 100%;
      padding: 0.625rem 0.75rem;
      border: 1px solid var(--surface-border);
      border-radius: var(--radius-md);
      background: var(--content-surface);
      font-size: 0.875rem;
      color: var(--text-primary);
      cursor: pointer;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
      text-align: left;
    }
    .cs__trigger:hover { border-color: var(--text-muted); }
    .cs--open .cs__trigger {
      border-color: var(--color-primary);
      box-shadow: 0 0 0 3px var(--color-primary-focus-ring);
    }
    .cs__trigger--placeholder { color: var(--text-muted); }
    .cs__trigger:disabled { opacity: 0.5; cursor: not-allowed; }
    .cs__trigger-text {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .cs__chevron {
      color: var(--text-muted);
      transition: transform var(--transition-fast);
      flex-shrink: 0;
      margin-left: 0.5rem;
    }
    .cs__chevron--open { transform: rotate(180deg); }
    .cs__dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      margin-top: 0.25rem;
      background: var(--content-surface);
      border: 1px solid var(--surface-border);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      z-index: 1000;
      max-height: 240px;
      overflow-y: auto;
      animation: csSlideDown 0.15s ease;
    }
    .cs__option {
      display: block;
      width: 100%;
      padding: 0.625rem 0.75rem;
      border: none;
      background: none;
      font-size: 0.875rem;
      color: var(--text-primary);
      text-align: left;
      cursor: pointer;
      transition: background var(--transition-fast);
    }
    .cs__option:hover { background: var(--surface-hover); }
    .cs__option--selected {
      background: var(--color-primary-tint);
      color: var(--color-primary);
      font-weight: 500;
    }
    .cs__empty {
      padding: 0.75rem;
      text-align: center;
      color: var(--text-muted);
      font-size: 0.8125rem;
    }
    @keyframes csSlideDown {
      from { opacity: 0; transform: translateY(-4px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `],
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
