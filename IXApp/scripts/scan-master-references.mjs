import fs from 'node:fs';
import path from 'node:path';
import ts from 'typescript';
const root=path.resolve('src/modules');
const files=[];
function walk(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const p=path.join(dir,e.name);if(e.isDirectory())walk(p);else if(e.name.endsWith('Page.tsx'))files.push(p);}}
walk(root);
const pages=files.map(file=>{const source=fs.readFileSync(file,'utf8');const tree=ts.createSourceFile(file,source,ts.ScriptTarget.Latest,true,ts.ScriptKind.TSX);const fields=[];function visit(node){if(ts.isObjectLiteralExpression(node)){const props=new Map(node.properties.filter(ts.isPropertyAssignment).map(p=>[p.name.getText(tree).replace(/['"]/g,''),p.initializer]));const name=props.get('name')??props.get('field');if(name&&ts.isStringLiteral(name)&&(props.has('label')||props.has('headerName'))){fields.push({name:name.text,line:tree.getLineAndCharacterOfPosition(node.getStart()).line+1,masterRoute:props.get('masterRoute')?.getText(tree)??null,previewEntity:props.get('previewEntity')?.getText(tree)??null,type:props.get('type')?.getText(tree)??null});}}ts.forEachChild(node,visit);}visit(tree);return {file:path.relative(process.cwd(),file).replaceAll('\\','/'),fields};});
const result={pageCount:pages.length,fieldCount:pages.reduce((n,p)=>n+p.fields.length,0),pages};
fs.writeFileSync('../.validation/master-reference-scan.json',JSON.stringify(result,null,2));
console.log(JSON.stringify({pageCount:result.pageCount,fieldCount:result.fieldCount,unlinked:pages.flatMap(p=>p.fields.filter(f=>!f.masterRoute&&/currency|taxGroup|taxItem|taxPeriod|taxAccountGroup|inventSite|inventLocation|custGroup|paymTerm|paymMode|invoiceAccount/i.test(f.name)).map(f=>({file:p.file,...f})))},null,2));
