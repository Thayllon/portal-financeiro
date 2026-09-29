import { TestBed } from '@angular/core/testing';
import { ConfirmDialogComponent } from './confirm-dialog.component';
import { ConfirmService } from '../services/confirm.service';

async function criarDialogo() {
  TestBed.configureTestingModule({
    imports: [ConfirmDialogComponent],
    providers: [ConfirmService]
  });
  TestBed.overrideComponent(ConfirmDialogComponent, { set: { template: '' } });
  await TestBed.compileComponents();
  const fixture = TestBed.createComponent(ConfirmDialogComponent);
  return { fixture, comp: fixture.componentInstance, service: TestBed.inject(ConfirmService) };
}

describe('ConfirmDialogComponent.texto', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('confirm simples libera Confirmar sem digitar', async () => {
    const { comp, service } = await criarDialogo();
    void service.confirm('Título', 'Mensagem?');

    expect(service.palavraChave()).toBeNull();
    expect(comp.podeConfirmar()).toBe(true);
  });

  it('confirm com texto exige SIM em qualquer caixa', async () => {
    const { comp, service } = await criarDialogo();
    void service.confirmComTexto('Excluir', 'Digite SIM.');

    expect(comp.podeConfirmar()).toBe(false);
    comp.texto.set('sim');
    expect(comp.podeConfirmar()).toBe(true);
    comp.texto.set('SIM');
    expect(comp.podeConfirmar()).toBe(true);
    comp.texto.set('  Sim  ');
    expect(comp.podeConfirmar()).toBe(true);
    comp.texto.set('não');
    expect(comp.podeConfirmar()).toBe(false);
  });

  it('accept resolve true e limpar fecha sem confirmar', async () => {
    const { service } = await criarDialogo();
    const promessa = service.confirmComTexto('Excluir', 'Digite SIM.');
    service.accept();

    await expect(promessa).resolves.toBe(true);
    expect(service.visible()).toBe(false);
    expect(service.palavraChave()).toBeNull();
  });
});
