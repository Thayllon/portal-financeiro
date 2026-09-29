import { TestBed } from '@angular/core/testing';
import { LancamentoModalComponent } from './lancamento-modal.component';
import { NotificationService } from '../../core/services/notification.service';
import { CategoriaReceitaRepository, CategoriaDespesaRepository, CategoriaServicoRepository } from '../../core/repositories/categoria.repository';
import { PessoaRepository } from '../../core/repositories/pessoa.repository';

const cloneSimples = {
  id: '',
  descricao: 'Contrato ativo',
  valor: 1000,
  data: '2026-09-27T00:00:00',
  idConta: 'conta-1',
  idCategoria: 'cat-1',
  idSubcategoria: 'sub-1'
};

async function criarModal() {
  TestBed.configureTestingModule({
    imports: [LancamentoModalComponent],
    providers: [
      { provide: NotificationService, useValue: { error: () => undefined, success: () => undefined, info: () => undefined } },
      { provide: CategoriaReceitaRepository, useValue: {} },
      { provide: CategoriaDespesaRepository, useValue: {} },
      { provide: CategoriaServicoRepository, useValue: {} },
      { provide: PessoaRepository, useValue: {} }
    ]
  });
  TestBed.overrideComponent(LancamentoModalComponent, { set: { template: '' } });
  await TestBed.compileComponents();
  return TestBed.createComponent(LancamentoModalComponent);
}

function abrirClone(fixture: ReturnType<typeof TestBed.createComponent<LancamentoModalComponent>>) {
  const comp = fixture.componentInstance;
  fixture.componentRef.setInput('categorias', [
    { id: 'cat-1', idUsuario: 'u1', nome: 'Empresa', ativo: true, podeEditar: true, dataCadastro: '' },
    { id: 'sub-1', idUsuario: 'u1', nome: 'Contrato ativo', categoriaPaiId: 'cat-1', ativo: true, podeEditar: true, dataCadastro: '' }
  ]);
  fixture.componentRef.setInput('categoriasServico', [
    { id: 'srv-1', idUsuario: 'u1', nome: 'Serviço', ativo: true, podeEditar: true, dataCadastro: '' }
  ]);
  fixture.componentRef.setInput('editando', cloneSimples);
  fixture.componentRef.setInput('visible', true);
  fixture.componentRef.setInput('fluxoAdicional', false);
  fixture.detectChanges();
  TestBed.flushEffects();
  return comp;
}

describe('LancamentoModalComponent.clone', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('abre o clone na Categoria com os dados copiados', async () => {
    const comp = abrirClone(await criarModal());
    expect(comp.passoAtual()).toBe(0);
    expect(comp.form().descricao).toBe('Contrato ativo');
    expect(comp.form().valor).toBe(1000);
    expect(comp.form().data).toBe('2026-09-27');
  });

  it('digitar em Dados nao volta de aba nem apaga o digitado', async () => {
    const comp = abrirClone(await criarModal());
    comp.irPara(1);
    expect(comp.passoAtual()).toBe(1);

    comp.updateFormField('descricao', 'Contrato ativo editado');
    comp.updateFormField('valor', 2000);
    comp.updateFormField('data', '2026-12-27');
    TestBed.flushEffects();

    expect(comp.passoAtual()).toBe(1);
    expect(comp.form().descricao).toBe('Contrato ativo editado');
    expect(comp.form().valor).toBe(2000);
    expect(comp.form().data).toBe('2026-12-27');
  });
});
