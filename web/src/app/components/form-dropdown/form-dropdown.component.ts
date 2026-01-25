import {Component, EventEmitter, Input, Output} from '@angular/core';
import {faChevronDown} from "@fortawesome/free-solid-svg-icons";

@Component({
    selector: 'form-dropdown',
    templateUrl: './form-dropdown.component.html'
})
export class FormDropdownComponent {
    _name: string = 'NAME NOT SET, FIX ME';
    _readonly: boolean = false;
    _options: DropdownOption[] = [];

    @Input() value: string = "";
    @Output() valueChange: EventEmitter<string> = new EventEmitter<string>;

    @Input()
    set name(param: string) {
        this._name = param;
    }

    @Input()
    set readonly(param: boolean) {
        this._readonly = param;
    }

    @Input()
    set options(options: DropdownOption[]) {
        this._options = options;
    }

    onChange(event: any) {
        this.value = event.target.value;
        this.valueChange.emit(this.value);
    }

    protected readonly faChevronDown = faChevronDown;
}

export interface DropdownOption {
    Value: string
    Name: string
}
