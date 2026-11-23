const fs = require('node:fs');
const path = require('node:path');
const ts = require('../IXApp/node_modules/typescript');
const root = path.resolve(__dirname, '../IXApp');
function walk(dir) { return fs.readdirSync(dir, {withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(dir,e.name)):[path.join(dir,e.name)]); }
const files = walk(path.join(root,'src/test')).filter(f=>/\.test\.tsx?$/.test(f)).sort();
const inventory = files.map(file=>{
 const text = fs.readFileSync(file,'utf8');
 const ast = ts.createSourceFile(file,text,ts.ScriptTarget.Latest,true);
 const imports=[], tests=[], mocks=[], hooks=[], assertions=[];
 function visit(n) {
  if(ts.isImportDeclaration(n)) imports.push(n.moduleSpecifier.text);
  if(ts.isCallExpression(n)) {
   const expr=n.expression.getText(ast);
   if(/^(it|test)(\.|$)/.test(expr) && n.arguments[0] && ts.isStringLiteralLike(n.arguments[0])) tests.push(n.arguments[0].text);
   if(expr==='vi.mock') mocks.push(n.arguments[0]?.getText(ast));
   if(/^(beforeEach|afterEach|beforeAll|afterAll)$/.test(expr)) hooks.push(n.getText(ast));
   if(expr.startsWith('expect(')) assertions.push(n.getText(ast));
  }
  ts.forEachChild(n,visit);
 }
 visit(ast);
 return {file:path.relative(root,file).replaceAll('\\','/'),imports,tests,mocks,hooks,assertions};
});
fs.writeFileSync(path.join(__dirname,'frontend-test-inventory.json'),JSON.stringify(inventory,null,2));
const start=Number(process.argv[2]||0), count=Number(process.argv[3]||20);
for(const f of inventory.slice(start,start+count)) console.log(JSON.stringify({file:f.file,imports:f.imports.filter(i=>i.startsWith('@modules')||i.startsWith('@shared')||i.startsWith('@patterns')||i.startsWith('.')),tests:f.tests,mocks:f.mocks,hooks:f.hooks.map(h=>h.replace(/\s+/g,' '))}));
